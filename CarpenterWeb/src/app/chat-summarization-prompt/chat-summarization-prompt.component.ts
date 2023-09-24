import { Component } from '@angular/core';
import { CarpenterApiService } from '../carpenter-api.service';

@Component({
  selector: 'app-chat-summarization-prompt',
  templateUrl: './chat-summarization-prompt.component.html',
  styleUrls: ['./chat-summarization-prompt.component.css']
})
export class ChatSummarizationPromptComponent {
  public chatSummarizationPrompt: string = "";

  constructor(private apiService: CarpenterApiService) {
  }

  async ngOnInit() {
    this.chatSummarizationPrompt = await this.apiService.getChatSummarizationPrompt();
  }

  public onChatSummarizationPromptValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.chatSummarizationPrompt = value;
  }

  onSetChatSummarizationPrompt(): void {
    this.apiService.setChatSummarizationPrompt(this.chatSummarizationPrompt);
  }
}
