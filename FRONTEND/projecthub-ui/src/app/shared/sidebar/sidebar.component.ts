import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationService, Notification } from '../../services/notification.service';
import { ChatService } from '../../services/chat.service';
import { Observable } from 'rxjs';

import { Router } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  userName: string = '';
  userRole: string = '';
  
  notifications$: Observable<Notification[]>;
  unreadCount$: Observable<number>;
  showNotifications: boolean = false;
  chatIsOpen: boolean = false;
  activeChatMode: string = '';

  constructor(
    private auth: AuthService,
    private themeService: ThemeService,
    private notificationService: NotificationService,
    private chatService: ChatService,
    private router: Router
  ) {
    this.notifications$ = this.notificationService.notifications$;
    this.unreadCount$ = this.notificationService.unreadCount$;
  }

  ngOnInit(): void {
    const user = this.auth.currentUserValue;
    this.userName = user?.user?.name || user?.name || '';
    this.userRole = this.auth.getUserRole();
    
    if (this.auth.isLoggedIn()) {
      this.notificationService.loadNotifications();
    }

    this.notificationService.showNotifications$.subscribe(show => {
      this.showNotifications = show;
    });

    this.chatService.isOpen$.subscribe(open => {
      this.chatIsOpen = open;
    });

    this.chatService.chatMode$.subscribe(mode => {
      this.activeChatMode = mode;
    });
  }

  toggleNotifications(): void {
    this.notificationService.toggleNotifications();
  }

  handleNotificationClick(n: Notification): void {

    if (!n.isRead) {
      this.notificationService.markAsRead(n.id).subscribe();
    }

    this.showNotifications = false;

    switch (n.type) {
      case 'Chat':
      case 'Task':
        this.router.navigate(['/tasks']);
        break;
      case 'Course':
        if (n.relatedId) {
          this.router.navigate(['/courses', n.relatedId]);
        } else {
          this.router.navigate(['/courses']);
        }
        break;
      case 'Certificate':
        this.router.navigate(['/certificates']);
        break;
      case 'Project':
        if (n.relatedId) {
          this.router.navigate(['/projects', n.relatedId]);
        } else {
          this.router.navigate(['/projects']);
        }
        break;
      default:

        this.router.navigate(['/dashboard']);
        break;
    }
  }

  markAsRead(id: number): void {
    this.notificationService.markAsRead(id).subscribe();
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe();
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  openAi(): void {
    this.chatService.openAiAssistant();
  }

  openTeam(): void {
    this.chatService.openTeamChat();
  }

  isRoute(path: string): boolean {
    return this.router.url.startsWith(path);
  }

  navigateTo(path: string): void {
    this.chatService.closeChat();
    this.router.navigate([path]);
  }

  logout(): void {
    this.auth.logout();
  }
}
