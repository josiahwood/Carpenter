import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { ChatMessage } from '../models/chat-message';

@Component({
  selector: 'app-chat-log',
  templateUrl: './chat-log.component.html',
  styleUrls: ['./chat-log.component.css']
})
export class ChatLogComponent {
  public chatMessages: ChatMessage[] = [];
  public userChatMessage: string = "";
  public isWaiting: boolean = false;

  constructor(private httpClient: HttpClient) {
  }

  async ngOnInit() {
    this.chatMessages = await this.getChatMessages();
  }

  async getChatMessages() {
    //return [
    //  {
    //    sender: "User",
    //    message: "Message"
    //  },
    //  {
    //    sender: "AI",
    //    message: "Message"
    //  },
    //  {
    //    sender: "User",
    //    message: "Message"
    //  },
    //  {
    //    sender: "AI",
    //    message: "Message"
    //  },
    //  {
    //    sender: "User",
    //    message: "Message"
    //  },
    //  {
    //    sender: "AI",
    //    message: "Message"
    //  },
    //  {
    //    sender: "User",
    //    message: "Message"
    //  },
    //  {
    //    sender: "AI",
    //    message: "Message"
    //  },
    //  {
    //    sender: "User",
    //    message: "Message"
    //  },
    //  {
    //    sender: "AI",
    //    message: "Message"
    //  }
    //];

    try {
      const response = await fetch('/api/GetChatMessages');
      const payload = await response.json();
      return payload;
    } catch (error) {
      console.error('No chat messages could be found');
      return [];
    }
  }

  public onUserChatMessageValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.userChatMessage = value;
  }

  async delay(ms: number) {
    await new Promise(resolve => setTimeout(() => resolve(""), ms)).then(() => console.log("fired"));
  }

  async onSendUserChatMessage() {
    console.log("onSendUserChatMessage");
    console.log(this.userChatMessage);

    this.isWaiting = true;

    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/SendUserChatMessage";
    var body = this.userChatMessage;
    this.httpClient.post(url, body).subscribe({
      next: (data) => {
        console.log(data);
      },
      error: (error) => {
        console.log('Log the error here: ', error);
        this.isWaiting = false;
      },
      complete: async () => {
        this.userChatMessage = "";
        await this.onGenerateAIChatMessage();
      }
    });
  }

  async onGenerateAIChatMessage() {
    console.log("onGenerateAIChatMessage");

    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GenerateAIChatMessage";

    this.httpClient.get(url).subscribe({
      next: (data) => {
        console.log(data);
      },
      error: (error) => {
        console.log('Log the error here: ', error);
        this.isWaiting = false;
      },
      complete: async () => {
        await this.onUpdateMessageGenerationStatus();
      }
    });
  }

  async onUpdateMessageGenerationStatus() {
    console.log("onUpdateMessageGenerationStatus");

    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetMessageGenerations";

    var messageGenerationData: string;

    this.httpClient.get(url).subscribe({
      next: (data) => {
        messageGenerationData = data as string;
        console.log(data);
      },
      error: (error) => {
        console.log('Log the error here: ', error);
        this.isWaiting = false;
      },
      complete: async () => {
        var values: Array<object> = JSON.parse(messageGenerationData) as Array<object>;

        if (values.length == 0) {
          this.chatMessages = await this.getChatMessages();
          this.isWaiting = false;
        }
        else {
          setTimeout(this.onUpdateMessageGenerationStatus, 1000);
        }
      }
    });
  }
}
