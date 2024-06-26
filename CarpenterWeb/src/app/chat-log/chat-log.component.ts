import { Component, ElementRef, ViewChild } from '@angular/core';
import { ChatMessage } from '../models/chat-message';
import { MessageGeneration } from '../models/message-generation'
import { CarpenterApiService } from '../carpenter-api.service';
import { MatDialog } from '@angular/material/dialog';
import { ChatMessageDialogComponent, ChatMessageDialogResult } from '../chat-message-dialog/chat-message-dialog.component';
import { isEmpty } from 'rxjs';

@Component({
  selector: 'app-chat-log',
  templateUrl: './chat-log.component.html',
  styleUrls: ['./chat-log.component.css']
})
export class ChatLogComponent {
  @ViewChild('chatLogDiv') chatLogDiv!: ElementRef;
  public chatMessages: ChatMessage[] = [];
  public userChatMessage: string = "";
  public isWaiting: boolean = false;

  constructor(private apiService: CarpenterApiService, public dialog: MatDialog) {
  }

  async ngOnInit() {
    this.chatMessages = await this.apiService.getChatMessages();
    setTimeout(() => {
      this.chatLogDiv.nativeElement.scrollTop = this.chatLogDiv.nativeElement.scrollHeight;
    }, 0);
    await this.onUpdateMessageGenerationStatus();
  }

  public onUserChatMessageValueChange(event: Event): void {
    const value = (event.target as any).value;
    this.userChatMessage = value;
  }

  async onChatMessageClicked(chatMessage: ChatMessage) {
    let dialogRef = this.dialog.open(ChatMessageDialogComponent, {
      data: chatMessage,
      height: '400px',
      width: '300px',
    });

    dialogRef.afterClosed().subscribe((result: ChatMessageDialogResult) => {
      if (result == ChatMessageDialogResult.Delete) {
        const index = this.chatMessages.indexOf(chatMessage);

        if (index > -1) {
          this.chatMessages.splice(index, 1);
        }
      }
    });
  }

  async onWinnerClicked(chatMessage: ChatMessage) {
    if (chatMessage.alternateGroupId == null || chatMessage.alternateGroupId == "") {
      return;
    }

    var winnerModel: string = (await this.apiService.getMessageGeneration(chatMessage.messageGenerationId)).model;
    
    for (var alternate of this.chatMessages) {
      if (alternate.id != chatMessage.id && alternate.alternateGroupId == chatMessage.alternateGroupId) {
        var loserModel: string = (await this.apiService.getMessageGeneration(alternate.messageGenerationId)).model;
        await this.apiService.compareModels(winnerModel, loserModel);
        await this.apiService.deleteChatMessage(alternate.id);

        const index = this.chatMessages.indexOf(alternate);

        if (index > -1) {
          this.chatMessages.splice(index, 1);
        }

        break;
      }
    }

    chatMessage.alternateGroupId = "";
    await this.apiService.editChatMessage(chatMessage);
  }

  async onSendUserChatMessage() {
    console.log("onSendUserChatMessage");
    console.log(this.userChatMessage);

    this.isWaiting = true;

    if (this.userChatMessage != "") {
      var chatMessage = await this.apiService.sendUserChatMessage(this.userChatMessage);
      this.userChatMessage = "";
      this.chatMessages.push(chatMessage);
      setTimeout(() => {
        this.chatLogDiv.nativeElement.scrollTop = this.chatLogDiv.nativeElement.scrollHeight;
      }, 0);
    }

    await this.apiService.generateAIChatMessage();
    await this.onUpdateMessageGenerationStatus();
  }

  async onGenerateUserChatMessage() {
    this.isWaiting = true;

    await this.apiService.generateUserChatMessage();
    await this.onUpdateMessageGenerationStatus();
  }

  async onUpdateMessageGenerationStatus() {
    var messageGenerations:MessageGeneration[] = await this.apiService.getNotDoneMessageGenerations();

    if (messageGenerations.length == 0) {
      this.chatMessages = await this.apiService.getChatMessages();
      this.isWaiting = false;

      setTimeout(() => {
        this.chatLogDiv.nativeElement.scrollTop = this.chatLogDiv.nativeElement.scrollHeight;
      }, 0);
    }
    else {
      this.isWaiting = true;
      setTimeout(async () => { await this.onUpdateMessageGenerationStatus(); }, 1000);
    }
  }
}
