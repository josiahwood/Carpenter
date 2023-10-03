import { Component, ElementRef, ViewChild } from '@angular/core';
import { ChatSummaryLog } from '../models/chat-summary-log';
import { CarpenterApiService } from '../carpenter-api.service';

@Component({
  selector: 'app-chat-summary-logs',
  templateUrl: './chat-summary-logs.component.html',
  styleUrls: ['./chat-summary-logs.component.css']
})
export class ChatSummaryLogsComponent {
  @ViewChild('chatSummaryLogsDiv') chatSummaryLogsDiv!: ElementRef;
  public chatSummaryLogs: ChatSummaryLog[] = [];

  constructor(private apiService: CarpenterApiService) {
  }

  async ngOnInit() {
    this.chatSummaryLogs = await this.apiService.getChatSummaryLogs();
    setTimeout(() => {
      this.chatSummaryLogsDiv.nativeElement.scrollTop = this.chatSummaryLogsDiv.nativeElement.scrollHeight;
    }, 0);
  }

  async onDeleteClicked(chatSummaryLog: ChatSummaryLog) {
    var chatSummaryId: string | undefined = chatSummaryLog.chatSummaryId

    if (chatSummaryId !== undefined) {
      await this.apiService.deleteChatSummary(chatSummaryId);
      this.chatSummaryLogs = await this.apiService.getChatSummaryLogs();
      setTimeout(() => {
        this.chatSummaryLogsDiv.nativeElement.scrollTop = this.chatSummaryLogsDiv.nativeElement.scrollHeight;
      }, 0);
    }
  }
}
