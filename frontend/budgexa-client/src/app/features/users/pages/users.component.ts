import { Component, inject, signal, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';
import { DataGridComponent } from '../../../shared/components/data-grid/data-grid.component';
import { GridRequestDto } from '../../../core/models/grid.model';
import { UsersGridService } from '../services/users-grid.service';
import { UserGridDto } from '../models/user-grid.model';
import { USERS_GRID_COLUMNS } from '../config/users-grid-columns.config';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, TranslateModule, UserMenuComponent, DataGridComponent],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  private readonly usersGridService = inject(UsersGridService);
  
  @ViewChild(DataGridComponent) grid?: DataGridComponent<UserGridDto>;
  
  protected readonly loading = signal(false);
  protected readonly columns = USERS_GRID_COLUMNS;

  ngOnInit(): void {}

  protected onGridStateChange(request: GridRequestDto): void {
    this.loading.set(true);
    
    this.usersGridService.getGrid(request).subscribe({
      next: (response) => {
        this.grid?.setData(response);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading users grid:', error);
        this.loading.set(false);
      }
    });
  }
}
