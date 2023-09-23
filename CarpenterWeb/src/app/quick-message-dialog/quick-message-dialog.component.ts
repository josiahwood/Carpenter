import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { QuickMessage } from '../models/quick-message';
import { CarpenterApiService } from '../carpenter-api.service';

@Component({
  selector: 'app-quick-message-dialog',
  templateUrl: './quick-message-dialog.component.html',
  styleUrls: ['./quick-message-dialog.component.css']
})
export class QuickMessageDialogComponent {
  public tempMessage: string;

  constructor(public dialogRef: MatDialogRef<QuickMessageDialogComponent>, @Inject(MAT_DIALOG_DATA) public data: QuickMessage, private apiService: CarpenterApiService) {
    this.tempMessage = data.message;
  }

  public onQuickMessageMessageValueChange(event: Event): void {
    const value = (event.target as any).value;
    this.tempMessage = value;
  }

  public async onUpdate() {
    this.data.message = this.tempMessage;
    await this.apiService.editQuickMessage(this.data);
    this.closeDialog();
  }

  closeDialog() {
    this.dialogRef.close();
  }
}
