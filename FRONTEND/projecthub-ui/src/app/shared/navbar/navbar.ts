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

    this.notificationService.notifications$.pipe(
      map(notifications => notifications.filter(n => n.type === 'Chat' && !n.isRead).length)
    ).subscribe(count => {
      this.unreadCount = count;
    });

    this.notificationService.unreadCount$.subscribe(count => {
      this.notificationCount = count;
    });

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
