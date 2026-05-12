import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class RealTimeChatService {
  private hubConnection!: signalR.HubConnection;
  private messageReceivedSubject = new BehaviorSubject<any>(null);
  public messageReceived$ = this.messageReceivedSubject.asObservable();

  private newMessageAlertSubject = new BehaviorSubject<any>(null);
  public newMessageAlert$ = this.newMessageAlertSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  private apiUrl = `${environment.apiUrl}/Message`;

  constructor(
    private http: HttpClient, 
    private auth: AuthService,
    private notificationService: NotificationService
  ) {
    this.startConnection();
    this.requestNotificationPermission();
    

    this.auth.currentUser.subscribe(user => {
      if (user && user.token && this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
        const userId = this.auth.getUserId();
        if (userId) {
          this.hubConnection.invoke('JoinChat', userId.toString())
            .then(() => {})
            .catch(err => {});
        }
      }
    });
  }

  private requestNotificationPermission() {
    if ('Notification' in window) {
      Notification.requestPermission();
    }
  }

  private startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api', '')}/chathub`, {
        accessTokenFactory: () => this.auth.getToken() || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        const userId = this.auth.getUserId();
        if (userId) {
          this.hubConnection.invoke('JoinChat', userId.toString())
            .then(() => {})
            .catch(err => {});
        }
      })
      .catch(err => {});

    this.hubConnection.on('ReceiveMessage', (senderId, messageData) => {
      const currentUserId = this.auth.getUserId();
      if (senderId && senderId.toString() === currentUserId.toString()) {
        return;
      }


      this.notificationService.loadNotifications();

      if (messageData && messageData.type === 'DELETED') {
        this.messageReceivedSubject.next({ type: 'DELETED', id: messageData.id, senderId });
        return;
      }

      let content = '';
      let fileUrl = null;
      
      if (typeof messageData === 'string') {
        content = messageData;
      } else if (messageData) {
        content = messageData.content || messageData.Content || '';
        fileUrl = messageData.fileUrl || messageData.FileUrl || null;
      }

      const msgData = { 
        id: messageData?.id || messageData?.Id || 0,
        senderId, 
        content, 
        fileUrl, 
        sentAt: new Date() 
      };
      this.messageReceivedSubject.next(msgData);
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
      this.updateTitle();
      this.showBrowserNotification(senderId, content);
      this.newMessageAlertSubject.next(msgData);
    });

    this.hubConnection.on('ReceiveNotification', (message: string) => {
      this.notificationService.loadNotifications();
      this.showBrowserNotification('System', message || 'You have a new notification');
      // Update unread count is handled inside notificationService.loadNotifications()
    });
  }

  private showBrowserNotification(senderId: string, content: string) {
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification('New Message', {
        body: content || 'Shared a file',
        icon: 'assets/chat-icon.png'
      });
    }
  }

  public getMessages(otherUserId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${otherUserId}`);
  }

  public sendMessage(receiverId: number, content: string): Observable<any> {
    return this.http.post<any>(this.apiUrl, { receiverId, content });
  }

  public sendMessageWithFile(receiverId: number, content: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('receiverId', receiverId.toString());
    formData.append('content', content);
    formData.append('file', file);
    return this.http.post<any>(`${this.apiUrl}/file`, formData);
  }

  public notifyRecipient(receiverId: number, messageData: any) {
    this.hubConnection.invoke('SendMessage', receiverId.toString(), messageData)
      .catch(err => console.error(err));
  }

  public deleteMessage(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  public notifyDelete(receiverId: number, messageId: number) {
    this.hubConnection.invoke('SendMessage', receiverId.toString(), { type: 'DELETED', id: messageId })
      .catch(err => console.error(err));
  }

  public clearUnreadCount() {
    this.unreadCountSubject.next(0);
    this.updateTitle();
  }

  private updateTitle() {
    const count = this.unreadCountSubject.value;
    if (count > 0) {
      document.title = `(${count}) ProjectHub`;
    } else {
      document.title = `ProjectHub`;
    }
  }
}
