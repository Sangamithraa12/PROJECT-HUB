import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeaderboardService, LeaderboardEntry } from '../../services/leaderboard.service';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './leaderboard.html',
  styleUrls: ['./leaderboard.css']
})
export class Leaderboard implements OnInit {
  leaderboard: LeaderboardEntry[] = [];
  isLoading: boolean = false;

  constructor(private leaderboardService: LeaderboardService) {}

  ngOnInit(): void {
    this.loadLeaderboard();
  }

  loadLeaderboard(): void {
    this.isLoading = true;
    this.leaderboardService.getLeaderboard().subscribe({
      next: (data) => {
        this.leaderboard = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading leaderboard', err);
        this.isLoading = false;
      }
    });
  }

  getRankIcon(index: number): string {
    switch(index) {
      case 0: return '🥇';
      case 1: return '🥈';
      case 2: return '🥉';
      default: return (index + 1).toString();
    }
  }
}
 
