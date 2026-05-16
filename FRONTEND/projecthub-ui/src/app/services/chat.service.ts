import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TaskItem } from '../models/project.model';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private isOpenSubject = new BehaviorSubject<boolean>(false);
  isOpen$ = this.isOpenSubject.asObservable();

  private activeTaskSubject = new BehaviorSubject<TaskItem | null>(null);
  activeTask$ = this.activeTaskSubject.asObservable();

  private chatModeSubject = new BehaviorSubject<'task' | 'ai' | 'dm'>('task');
  chatMode$ = this.chatModeSubject.asObservable();

  openChat(task: TaskItem | null = null): void {
    this.activeTaskSubject.next(task);
    this.chatModeSubject.next('task');
    this.isOpenSubject.next(true);
  }

  openAiAssistant(): void {
    this.activeTaskSubject.next(null);
    this.chatModeSubject.next('ai');
    this.isOpenSubject.next(true);
  }

  openTeamChat(): void {
    this.activeTaskSubject.next(null);
    this.chatModeSubject.next('dm');
    this.isOpenSubject.next(true);
  }

  closeChat(): void {
    this.isOpenSubject.next(false);
  }

  toggleChat(): void {
    this.isOpenSubject.next(!this.isOpenSubject.value);
  }
}
 
