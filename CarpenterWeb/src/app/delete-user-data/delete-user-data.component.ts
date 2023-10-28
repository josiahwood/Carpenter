import { Component } from '@angular/core';
import { CarpenterApiService } from '../carpenter-api.service';

@Component({
  selector: 'app-delete-user-data',
  templateUrl: './delete-user-data.component.html',
  styleUrls: ['./delete-user-data.component.css']
})
export class DeleteUserDataComponent {
  constructor(private apiService: CarpenterApiService) {
  }

  async onDeleteUserData() {
    await this.apiService.deleteCarpenterUser();
  }
}
