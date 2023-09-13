import { Component } from '@angular/core';
import { NameService } from '../carpenter-api-client';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-chat-memory',
  templateUrl: './chat-memory.component.html',
  styleUrls: ['./chat-memory.component.css']
})
export class ChatMemoryComponent {
  public chatMemory: string = "";

  constructor(private nameService: NameService, private httpClient: HttpClient) {
  }

  async ngOnInit() {
    this.chatMemory = await this.getChatMemory();
  }

  async getChatMemory() {
    try {
      const response = await fetch('/api/GetChatMemory');
      const payload = await response.text();
      return payload;
    } catch(error) {
      console.error('No chat memory could be found');
      return "";
    }
  }

  public onChatMemoryValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.chatMemory = value;
  }

  onSetMemory(): void {
    console.log("onSetMemory");
    console.log(this.chatMemory);

    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/SetChatMemory";
    var body = this.chatMemory;
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
    //this.nameService.run_1(this.chatMemory);
  }
}
