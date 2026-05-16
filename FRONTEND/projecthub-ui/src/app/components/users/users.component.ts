import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { SidebarComponent } from '../../shared/sidebar/sidebar.component';
import { of } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, SidebarComponent],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {
  users: any[] = [];
  roles: any[] = [];
  userRole: string = '';
  showCreateModal: boolean = false;
  showEditModal: boolean = false;
  
  newUser: any = { name: '', email: '', password: '', roleId: null };
  editingUser: any = { id: 0, name: '', email: '', roleId: null };

  constructor(
    private userService: UserService,
    public auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.userRole = this.auth.getUserRole();
    this.loadUsers();
    this.loadRoles();
  }

  loadUsers(): void {
    this.userService.getUsers().pipe(
      timeout(10000),
      catchError(err => {
        console.error('User loading failed:', err);
        return of([]);
      })
    ).subscribe((data: any[]) => {
      this.users = data.map(u => ({
        id: u.id || u.Id,
        name: u.name || u.Name || 'Unknown User',
        email: u.email || u.Email || '',
        roleName: u.roleName || u.RoleName || (u.role && u.role.name) || (u.Role && u.Role.Name) || 'Member'
      }));
      this.cdr.detectChanges();
    });
  }

  loadRoles(): void {
    this.userService.getRoles().pipe(
      timeout(8000),
      catchError(err => {
        console.error('Roles loading failed:', err);
        return of([]);
      })
    ).subscribe((data: any) => {
      this.roles = data;
      this.cdr.detectChanges();
    });
  }

  createUser(): void {
    if (this.newUser.name && this.newUser.email && this.newUser.password && this.newUser.roleId) {
      this.userService.createUser(this.newUser).subscribe({
        next: () => {
          this.loadUsers();
          this.showCreateModal = false;
          this.newUser = { name: '', email: '', password: '', roleId: null };
          alert('Account created successfully!');
        },
        error: (err) => {
          console.error('Account creation failed:', err);
          alert('Failed to create account. Email might already be in use.');
        }
      });
    } else {
      alert('Please fill in all fields and select a role.');
    }
  }

  openEditModal(user: any): void {
    this.editingUser = { 
      id: user.id, 
      name: user.name, 
      email: user.email,
      roleId: user.roleId || (user.role && user.role.id) || 3 
    };
    this.showEditModal = true;
  }

  updateUser(): void {
    if (this.editingUser.name && this.editingUser.email && this.editingUser.roleId) {
      this.userService.updateUser(this.editingUser.id, this.editingUser).subscribe({
        next: () => {
          this.loadUsers();
          this.showEditModal = false;
          alert('User updated successfully!');
        },
        error: (err) => {
          console.error('Update failed:', err);
          alert('Failed to update user.');
        }
      });
    }
  }

  deleteUser(id: number): void {
    if (confirm('Are you sure you want to delete this user?')) {
      this.userService.deleteUser(id).subscribe({
        next: (res: any) => {
          alert(res?.message || 'User deleted successfully');
          this.loadUsers();
        },
        error: (err: any) => {
          console.error('Delete failed:', err);
          const errorMsg = err.error?.message || 'Failed to delete user. They might have related data.';
          alert(errorMsg);
        }
      });
    }
  }

  logout(): void {
    this.auth.logout();
  }
}
