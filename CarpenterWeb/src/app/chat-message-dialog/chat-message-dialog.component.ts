import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ChatMessage } from '../models/chat-message';
import { CarpenterApiService } from '../carpenter-api.service';

@Component({
  selector: 'app-chat-message-dialog',
  templateUrl: './chat-message-dialog.component.html',
  styleUrls: ['./chat-message-dialog.component.css']
})
export class ChatMessageDialogComponent {
  public tempMessage: string;

  constructor(public dialogRef: MatDialogRef<ChatMessageDialogComponent>, @Inject(MAT_DIALOG_DATA) public data: ChatMessage, private apiService: CarpenterApiService) {
    this.tempMessage = data.message;
  }

  public onChatMessageMessageValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.tempMessage = value;
  }

  public async onUpdate() {
    this.data.message = this.tempMessage;
    await this.apiService.editChatMessage(this.data);
    this.closeDialog();
  }

  closeDialog() {
    this.dialogRef.close();
  }
}
