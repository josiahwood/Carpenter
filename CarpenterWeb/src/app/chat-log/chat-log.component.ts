import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-chat-log',
  templateUrl: './chat-log.component.html',
  styleUrls: ['./chat-log.component.css']
})
export class ChatLogComponent {
  public userChatMessage: string = "";

  constructor(private httpClient: HttpClient) {
  }

  public onUserChatMessageValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.userChatMessage = value;
  }

  onSendUserChatMessage(): void {
    console.log("onSendUserChatMessage");
    console.log(this.userChatMessage);

    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/SendUserChatMessage";
    var body = this.userChatMessage;
    this.httpClient.post(url, body).subscribe({
      next: (data) => {
        console.log(data);
      },
      error: (error) => {
        console.log('Log the error here: ', error);
      },
      complete: () => {
      }
    });
  }
}
