import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Notification {
  id: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  relatedId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/Notification`;
  private showNotificationsSubject = new BehaviorSubject<boolean>(false);
  public showNotifications$ = this.showNotificationsSubject.asObservable();
  
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();
  
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  toggleNotifications(): void {
    this.showNotificationsSubject.next(!this.showNotificationsSubject.value);
  }

  constructor(private http: HttpClient) {}

  loadNotifications(): void {
    this.http.get<Notification[]>(this.apiUrl).subscribe(data => {
      this.notificationsSubject.next(data);
      this.updateUnreadCount(data);
    });
  }

  private updateUnreadCount(notifications: Notification[]): void {
    const unread = notifications.filter(n => !n.isRead).length;
    this.unreadCountSubject.next(unread);
  }

  markAsRead(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => {
        const current = this.notificationsSubject.value;
        const updated = current.map(n => n.id === id ? { ...n, isRead: true } : n);
        this.notificationsSubject.next(updated);
        this.updateUnreadCount(updated);
      })
    );
  }

  markAllAsRead(): Observable<any> {
    return this.http.put(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => {
        const current = this.notificationsSubject.value;
        const updated = current.map(n => ({ ...n, isRead: true }));
        this.notificationsSubject.next(updated);
        this.unreadCountSubject.next(0);
      })
    );
  }

  markAllChatAsRead(): void {
    const current = this.notificationsSubject.value;
    const chatNotifs = current.filter(n => n.type === 'Chat' && !n.isRead);


    chatNotifs.forEach(n => this.markAsRead(n.id).subscribe());
  }
}
