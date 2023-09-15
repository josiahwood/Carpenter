import { Component, ElementRef, ViewChild } from '@angular/core';
import { ChatMessage } from '../models/chat-message';
import { MessageGeneration } from '../models/message-generation'
import { CarpenterApiService } from '../carpenter-api.service';

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

  constructor(private apiService: CarpenterApiService) {
  }

  async ngOnInit() {
    this.chatMessages = await this.apiService.getChatMessages();
    setTimeout(() => {
      this.chatLogDiv.nativeElement.scrollTop = this.chatLogDiv.nativeElement.scrollHeight;
    }, 0);
  }

  public onUserChatMessageValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.userChatMessage = value;
  }

  async onSendUserChatMessage() {
    console.log("onSendUserChatMessage");
    console.log(this.userChatMessage);

    this.isWaiting = true;

    await this.apiService.sendUserChatMessage(this.userChatMessage);
    this.userChatMessage = "";
    await this.apiService.generateAIChatMessage();
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
      setTimeout(async () => { await this.onUpdateMessageGenerationStatus(); }, 1000);
    }
  }
}
