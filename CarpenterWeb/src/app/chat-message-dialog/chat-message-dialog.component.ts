import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ChatMessage } from '../models/chat-message';

@Component({
  selector: 'app-chat-message-dialog',
  templateUrl: './chat-message-dialog.component.html',
  styleUrls: ['./chat-message-dialog.component.css']
})
export class ChatMessageDialogComponent {
  constructor(public dialogRef: MatDialogRef<ChatMessageDialogComponent>, @Inject(MAT_DIALOG_DATA) public data: ChatMessage) { }

  closeDialog() {
    this.dialogRef.close();
  }
}
