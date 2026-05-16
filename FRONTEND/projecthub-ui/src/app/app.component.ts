import { Component } from '@angular/core';
import { ChildrenOutletContexts, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ChatBotComponent } from './shared/chatbot/chatbot.component';
import { RealTimeChatService } from './services/real-time-chat.service';
import { UserService } from './services/user.service';
import { routeTransitionAnimations } from './app.animations';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ChatBotComponent, CommonModule],
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
  animations: [routeTransitionAnimations]
})
export class AppComponent {
  title = 'ProjectHub';
  activeToast: any = null;
  showToast: boolean = false;

  constructor(
    private realTimeChat: RealTimeChatService,
    private userService: UserService,
    private contexts: ChildrenOutletContexts
  ) {
    this.realTimeChat.newMessageAlert$.subscribe(msg => {
      if (msg) {
        this.userService.getUsers().subscribe(users => {
          const sender = users.find(u => u.id.toString() == msg.senderId.toString());
          this.activeToast = {
            senderName: sender ? sender.name : 'Team Member',
            content: msg.content
          };
          this.showToast = true;
          
        
          setTimeout(() => {
            this.showToast = false;
            setTimeout(() => this.activeToast = null, 500);
          }, 5000);
        });
      }
    });
  }

  prepareRoute() {
    return this.contexts.getContext('primary')?.route?.snapshot?.data?.['animation'];
  }
}
 
