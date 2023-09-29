import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ChatMessage } from '../models/chat-message';
import { CarpenterApiService } from '../carpenter-api.service';

export enum ChatMessageDialogResult {
  Update,
  Delete
}

@Component({
  selector: 'app-chat-message-dialog',
  templateUrl: './chat-message-dialog.component.html',
  styleUrls: ['./chat-message-dialog.component.css']
})
export class ChatMessageDialogComponent {
  public tempMessage: string;
  public model: string;

  constructor(public dialogRef: MatDialogRef<ChatMessageDialogComponent>, @Inject(MAT_DIALOG_DATA) public data: ChatMessage, private apiService: CarpenterApiService) {
    this.tempMessage = data.message;
    this.model = ""
  }

  async ngOnInit() {
    this.model = (await this.apiService.getMessageGeneration(this.data.messageGenerationId)).model
  }

  public onChatMessageMessageValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.tempMessage = value;
  }

  public async onUpdate() {
    this.data.message = this.tempMessage;
    await this.apiService.editChatMessage(this.data);
    this.dialogRef.close(ChatMessageDialogResult.Update);
  }

  public async onDelete() {
    await this.apiService.deleteChatMessage(this.data.id);
    this.dialogRef.close(ChatMessageDialogResult.Delete);
  }
}
