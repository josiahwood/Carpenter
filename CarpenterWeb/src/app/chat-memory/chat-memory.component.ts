import { Component } from '@angular/core';
import { CarpenterApiService } from '../carpenter-api.service';

@Component({
  selector: 'app-chat-memory',
  templateUrl: './chat-memory.component.html',
  styleUrls: ['./chat-memory.component.css']
})
export class ChatMemoryComponent {
  public chatMemory: string = "";

  constructor(private apiService: CarpenterApiService) {
  }

  async ngOnInit() {
    this.chatMemory = await this.apiService.getChatMemory();
  }

  public onChatMemoryValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.chatMemory = value;
  }

  onSetMemory(): void {
    this.apiService.setChatMemory(this.chatMemory);
  }
}
