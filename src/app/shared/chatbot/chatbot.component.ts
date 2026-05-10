import { Component, OnInit, OnDestroy, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../services/chat.service';
import { TaskService } from '../../services/task.service';
import { AuthService } from '../../services/auth.service';
import { AiService } from '../../services/ai.service';
import { UserService } from '../../services/user.service';
import { RealTimeChatService } from '../../services/real-time-chat.service';
import { NotificationService } from '../../services/notification.service';
import { TaskItem, Comment } from '../../models/project.model';
import { Subscription, map, take } from 'rxjs';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot.component.html',
  styleUrls: ['./chatbot.component.css']
})
export class ChatBotComponent implements OnInit, OnDestroy {
  isOpen: boolean = false;
  activeTask: TaskItem | null = null;
  allTasks: TaskItem[] = [];
  isAiMode: boolean = false;
  aiMessages: any[] = [];
  
  // Team Chat Mode
  isDmMode: boolean = false;
  isTeamOnlyMode: boolean = false;
  activeChatUser: any = null;
  teamMembers: any[] = [];
  dmMessages: { [userId: number]: any[] } = {};
  unreadCounts: { [userId: number]: number } = {};

  currentUserId: number = 0;
  chatCommentInput: string = '';
  isSendingComment: boolean = false;
  isUploadingChatFile: boolean = false;
  chatFileProgress: number = 0;
  openMenuId: number | null = null;

  private subs = new Subscription();

  @ViewChild('chatScroll') private chatScrollContainer!: ElementRef;
  @ViewChild('chatFileInput') chatFileInput!: ElementRef;

  constructor(
    private chatService: ChatService,
    private taskService: TaskService,
    private auth: AuthService,
    private aiService: AiService,
    private userService: UserService,
    private realTimeChat: RealTimeChatService,
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.subs.add(this.auth.currentUser.subscribe(user => {
      this.currentUserId = this.auth.getUserId();
      this.cdr.detectChanges();
    }));

    // Track unread counts per user from notifications
    this.subs.add(this.notificationService.notifications$.pipe(
      map(notifications => {
        const counts: { [userId: number]: number } = {};
        notifications.filter(n => n.type === 'Chat' && !n.isRead).forEach(n => {
          if (n.relatedId) {
            counts[n.relatedId] = (counts[n.relatedId] || 0) + 1;
          }
        });
        return counts;
      })
    ).subscribe(counts => {
      this.unreadCounts = counts;
      this.cdr.detectChanges();
    }));

    this.subs.add(this.chatService.isOpen$.subscribe(open => {
      this.isOpen = open;
      if (open) {
        this.currentUserId = this.auth.getUserId(); // Refresh on open
        setTimeout(() => this.scrollToBottom(), 100);
      }
    }));

    this.subs.add(this.chatService.activeTask$.subscribe(task => {
      this.activeTask = task;
      this.isAiMode = false;
      this.isDmMode = false; // Reset other modes
      if (task) {
        setTimeout(() => this.scrollToBottom(), 100);
      } else {
        this.loadAllTasks();
        this.loadTeamMembers();
      }
    }));

    this.subs.add(this.realTimeChat.messageReceived$.subscribe(msg => {
      if (!msg) return;

      if (msg.type === 'DELETED') {
        const userId = +msg.senderId;
        if (this.dmMessages[userId]) {
          this.dmMessages[userId] = this.dmMessages[userId].filter(m => m.id !== msg.id);
          this.cdr.detectChanges();
        }
        return;
      }

      if (this.isDmMode && msg.senderId == this.activeChatUser?.id) {
        if (!this.dmMessages[this.activeChatUser.id]) this.dmMessages[this.activeChatUser.id] = [];
        this.dmMessages[this.activeChatUser.id].push({
          id: msg.id,
          userName: this.activeChatUser.name,
          content: msg.content,
          fileUrl: msg.fileUrl,
          createdAt: msg.sentAt,
          userId: +msg.senderId
        });
        setTimeout(() => this.scrollToBottom(), 100);
        this.cdr.detectChanges();
      }
    }));

    this.subs.add(this.chatService.chatMode$.subscribe(mode => {
      if (mode === 'ai') {
        this.enterAiMode();
        this.isTeamOnlyMode = false;
      } else if (mode === 'dm') {
        this.isTeamOnlyMode = true;
        if (!this.activeChatUser) {
          this.isDmMode = false;
          this.isAiMode = false;
          this.activeTask = null;
        } else {
          this.enterDmMode(this.activeChatUser);
        }
      } else {
        this.isTeamOnlyMode = false;
      }
      this.cdr.detectChanges();
    }));
  }

  loadAllTasks(): void {
    this.taskService.getTasks().subscribe(tasks => {
      this.allTasks = tasks.filter(t => t.status !== 'Completed');
      this.cdr.detectChanges();
    });
  }

  selectTask(task: TaskItem | null): void {
    if (task === null) {
      this.activeTask = null;
      this.isAiMode = false;
      this.isDmMode = false;
      this.activeChatUser = null;
    } else {
      this.chatService.openChat(task);
    }
  }

  loadTeamMembers(): void {
    this.userService.getUsers().subscribe(users => {
      const myId = this.auth.getUserId();
      this.teamMembers = users.filter(u => u.id !== myId);
      this.cdr.detectChanges();
    });
  }

  enterDmMode(user: any): void {
    this.isDmMode = true;
    this.activeChatUser = user;
    this.isAiMode = false;
    this.activeTask = null;

    // Clear notifications from this specific user
    this.notificationService.notifications$.pipe(
      take(1),
      map(notifs => notifs.filter(n => n.type === 'Chat' && !n.isRead && n.relatedId === user.id))
    ).subscribe(unreadNotifs => {
      unreadNotifs.forEach(n => this.notificationService.markAsRead(n.id).subscribe());
    });
    
    this.realTimeChat.getMessages(user.id).subscribe(messages => {
      this.dmMessages[user.id] = messages.map(m => {
        const sId = m.senderId || m.SenderId || (m.userId !== undefined ? m.userId : null);
        const isMe = +sId === +this.currentUserId;
        return {
          id: m.id || m.Id,
          userName: isMe ? 'You' : user.name,
          content: m.content || m.Content,
          fileUrl: m.fileUrl || m.FileUrl,
          createdAt: m.sentAt || m.SentAt,
          userId: sId
        };
      });
      if (this.dmMessages[user.id].length === 0) {
        this.dmMessages[user.id] = [{
          userName: user.name,
          content: `Hi! This is the start of your chat with ${user.name}.`,
          createdAt: new Date(),
          userId: user.id
        }];
      }
      setTimeout(() => this.scrollToBottom(), 100);
      this.cdr.detectChanges();
    });
  }

  enterAiMode(): void {
    this.isAiMode = true;
    this.activeTask = null;
    if (this.aiMessages.length === 0) {
      this.aiMessages.push({
        userName: 'AI Assistant',
        content: "Hello! I'm your ProjectHub AI. You can ask me anything about the platform, your tasks, or just say hi!",
        createdAt: new Date(),
        userId: 0
      });
    }
    setTimeout(() => this.scrollToBottom(), 100);
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  closeChat(): void {
    this.openMenuId = null;
    this.chatService.closeChat();
  }

  toggleMsgMenu(id: number): void {
    this.openMenuId = this.openMenuId === id ? null : id;
  }

  private scrollToBottom(): void {
    if (this.chatScrollContainer) {
      this.chatScrollContainer.nativeElement.scrollTop = this.chatScrollContainer.nativeElement.scrollHeight;
    }
  }

  addChatComment(): void {
    if (!this.chatCommentInput.trim() || this.isSendingComment) return;

    if (this.isDmMode) {
      this.handleDmChat();
      return;
    }

    if (this.isAiMode) {
      this.handleAiChat();
      return;
    }

    if (!this.activeTask) return;

    this.isSendingComment = true;
    const taskId = this.activeTask.id;
    const content = this.chatCommentInput.trim();

    this.taskService.addComment(taskId, content).subscribe({
      next: (newComment) => {
        if (this.activeTask) {
          if (!this.activeTask.comments) this.activeTask.comments = [];
          this.activeTask.comments.push(newComment);
          this.chatCommentInput = '';
          setTimeout(() => this.scrollToBottom(), 100);
        }
        this.isSendingComment = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Chat failed:', err);
        this.isSendingComment = false;
        this.cdr.detectChanges();
      }
    });
  }

  private handleDmChat(): void {
    const userMsg = this.chatCommentInput.trim();
    const receiverId = this.activeChatUser.id;
    
    this.isSendingComment = true;
    this.chatCommentInput = ''; // Clear early for better feel
    
    this.realTimeChat.sendMessage(receiverId, userMsg).subscribe({
      next: (res) => {
        if (!this.dmMessages[receiverId]) this.dmMessages[receiverId] = [];
        
        this.dmMessages[receiverId].push({
          id: res.id || res.Id,
          userName: 'You',
          content: userMsg,
          createdAt: new Date(),
          userId: this.currentUserId
        });
        
        this.realTimeChat.notifyRecipient(receiverId, res); // Send the full response object
        this.isSendingComment = false;
        setTimeout(() => this.scrollToBottom(), 100);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('DM Failed:', err);
        alert('Could not send message. Please ensure the backend is running with the new Message table.');
        this.chatCommentInput = userMsg; // Restore text on error
        this.isSendingComment = false;
        this.cdr.detectChanges();
      }
    });
  }

  private handleAiChat(): void {
    const userMsg = this.chatCommentInput.trim();
    this.aiMessages.push({
      userName: 'You',
      content: userMsg,
      createdAt: new Date(),
      userId: this.currentUserId
    });
    this.chatCommentInput = '';
    this.isSendingComment = true;
    setTimeout(() => this.scrollToBottom(), 100);

    this.aiService.getAiResponse(userMsg).subscribe(response => {
      this.aiMessages.push({
        userName: 'AI Assistant',
        content: response,
        createdAt: new Date(),
        userId: 0
      });
      this.isSendingComment = false;
      this.cdr.detectChanges();
      setTimeout(() => this.scrollToBottom(), 100);
    });
  }

  triggerChatFileSelect(): void {
    this.chatFileInput.nativeElement.click();
  }

  onChatFileSelected(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    if (this.isDmMode && this.activeChatUser) {
      this.handleDmFileUpload(file);
      return;
    }

    if (!this.activeTask) return;

    this.isUploadingChatFile = true;
    this.chatFileProgress = 0;

    this.taskService.addCommentWithFile(this.activeTask.id, 'Shared a file', file).subscribe({
      next: (newComment) => {
        if (this.activeTask) {
          if (!this.activeTask.comments) this.activeTask.comments = [];
          this.activeTask.comments.push(newComment);
          setTimeout(() => this.scrollToBottom(), 100);
        }
        this.isUploadingChatFile = false;
        this.chatFileProgress = 0;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('File upload failed:', err);
        this.isUploadingChatFile = false;
        this.cdr.detectChanges();
      }
    });
  }

  private handleDmFileUpload(file: File): void {
    this.isUploadingChatFile = true;
    this.chatFileProgress = 10;
    const receiverId = this.activeChatUser.id;

    this.realTimeChat.sendMessageWithFile(receiverId, 'Shared a file', file).subscribe({
      next: (res) => {
        if (!this.dmMessages[receiverId]) this.dmMessages[receiverId] = [];
        
        this.dmMessages[receiverId].push({
          id: res.id || res.Id,
          userName: 'You',
          content: res.content,
          fileUrl: res.fileUrl,
          createdAt: new Date(),
          userId: this.currentUserId
        });

        this.realTimeChat.notifyRecipient(receiverId, res);
        this.isUploadingChatFile = false;
        setTimeout(() => this.scrollToBottom(), 100);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('DM File upload failed:', err);
        this.isUploadingChatFile = false;
        this.cdr.detectChanges();
      }
    });
  }

  deleteComment(commentId: number): void {
    if (!confirm('Delete this message?')) return;
    this.taskService.deleteComment(commentId).subscribe(() => {
      if (this.activeTask) {
        this.activeTask.comments = this.activeTask.comments?.filter(c => c.id !== commentId);
      }
      this.openMenuId = null;
      this.cdr.detectChanges();
    });
  }

  deleteDmMessage(msgId: number): void {
    if (!confirm('Delete this message?')) return;
    
    this.realTimeChat.deleteMessage(msgId).subscribe({
      next: () => {
        const receiverId = this.activeChatUser.id;
        this.dmMessages[receiverId] = this.dmMessages[receiverId].filter(m => m.id !== msgId);
        this.realTimeChat.notifyDelete(receiverId, msgId);
        this.openMenuId = null;
        this.cdr.detectChanges();
      }
    });
  }

  isImage(url: string): boolean {
    if (!url) return false;
    const ext = url.split('.').pop()?.toLowerCase();
    return ['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext || '');
  }

  getFileIcon(url: string): string {
    if (this.isImage(url)) return '🖼️';
    if (url.includes('.pdf')) return '📄';
    if (url.includes('.zip') || url.includes('.rar')) return '📦';
    return '📎';
  }

  getFileName(url: string): string {
    if (!url) return '';
    return url.split('/').pop()?.split('_').pop() || 'File';
  }

  getFullUrl(path: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return `${environment.fileBaseUrl}/${path}`;
  }

  openImage(url: string): void {
    window.open(url, '_blank');
  }
}
