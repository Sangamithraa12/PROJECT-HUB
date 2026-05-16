import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ChatService } from '../../services/chat.service';
import { AuthService } from '../../services/auth.service';
import { RealTimeChatService } from '../../services/real-time-chat.service';
import { NotificationService } from '../../services/notification.service';
import { map } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class NavbarComponent implements OnInit {
  userName: string = '';
  unreadCount: number = 0;
  notificationCount: number = 0;

  constructor(
    private chatService: ChatService,
    private auth: AuthService,
    private realTimeChat: RealTimeChatService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const user = this.auth.currentUserValue;
    this.userName = user?.user?.name || 'User';

    // Combine real-time count with persistent notification count for Chat type
    this.notificationService.notifications$.pipe(
      map(notifications => notifications.filter(n => n.type === 'Chat' && !n.isRead).length)
    ).subscribe(count => {
      this.unreadCount = count;
    });

    // Listen to all notifications for the bell icon
    this.notificationService.unreadCount$.subscribe(count => {
      this.notificationCount = count;
    });

    // Also listen to real-time service for immediate feedback
    this.realTimeChat.unreadCount$.subscribe(count => {
      if (count > 0) {
        this.unreadCount += count;
      }
    });
  }

  openChat(): void {
    this.realTimeChat.clearUnreadCount();
    this.chatService.openTeamChat();
  }

  openAi(): void {
    this.chatService.openAiAssistant();
  }

  toggleNotifications(): void {
    this.notificationService.toggleNotifications();
  }
}
 
