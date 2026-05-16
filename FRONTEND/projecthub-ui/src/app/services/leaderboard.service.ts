import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LeaderboardEntry {
  userId: number;
  userName: string;
  coursesCompleted: number;
  totalProgress: number;
  avgQuizScore: number;
}

@Injectable({
  providedIn: 'root'
})
export class LeaderboardService {
  private apiUrl = `${environment.apiUrl}/Leaderboard`;

  constructor(private http: HttpClient) { }

  getLeaderboard(): Observable<LeaderboardEntry[]> {
    return this.http.get<LeaderboardEntry[]>(this.apiUrl);
  }
}
 
