import { Component, ElementRef, ViewChild } from '@angular/core';
import { CarpenterApiService } from '../carpenter-api.service';
import { ChatContext } from '../models/chat-context';

@Component({
  selector: 'app-chat-contexts',
  templateUrl: './chat-contexts.component.html',
  styleUrls: ['./chat-contexts.component.css']
})
export class ChatContextsComponent {
  @ViewChild('chatContextsDiv') chatContextsDiv!: ElementRef;
  public chatContexts: ChatContext[] = [];

  constructor(private apiService: CarpenterApiService) {
  }

  async ngOnInit() {
    this.chatContexts = await this.apiService.getChatContexts();
    setTimeout(() => {
      this.chatContextsDiv.nativeElement.scrollTop = this.chatContextsDiv.nativeElement.scrollHeight;
    }, 0);
  }

  async onCreateNewChatContext() {
    var chatContext: ChatContext = <ChatContext>{};
    chatContext = await this.apiService.createChatContext(chatContext);
  }
}
