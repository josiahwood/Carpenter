import { Component, ElementRef, ViewChild } from '@angular/core';
import { QuickMessage } from '../models/quick-message';
import { CarpenterApiService } from '../carpenter-api.service';
import { MatDialog } from '@angular/material/dialog';
import { QuickMessageDialogComponent } from '../quick-message-dialog/quick-message-dialog.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-quick-messages',
  templateUrl: './quick-messages.component.html',
  styleUrls: ['./quick-messages.component.css']
})
export class QuickMessagesComponent {
  @ViewChild('quickMessagesDiv') quickMessagesDiv!: ElementRef;
  public quickMessages: QuickMessage[] = [];
  public newQuickMessage: string = "";

  constructor(private apiService: CarpenterApiService, public dialog: MatDialog, private router:Router) {
  }

  async ngOnInit() {
    this.quickMessages = await this.apiService.getQuickMessages();
    setTimeout(() => {
      this.quickMessagesDiv.nativeElement.scrollTop = this.quickMessagesDiv.nativeElement.scrollHeight;
    }, 0);
  }

  public onNewQuickMessageValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.newQuickMessage = value;
  }

  async onEditQuickMessage(quickMessage: QuickMessage) {
    let dialogRef = this.dialog.open(QuickMessageDialogComponent, {
      data: quickMessage,
      height: '400px',
      width: '300px',
    });
  }

  async onNewQuickMessage() {
    console.log(this.newQuickMessage);

    await this.apiService.createQuickMessage(this.newQuickMessage);
    this.newQuickMessage = "";
    this.quickMessages = await this.apiService.getQuickMessages();
    setTimeout(() => {
      this.quickMessagesDiv.nativeElement.scrollTop = this.quickMessagesDiv.nativeElement.scrollHeight;
    }, 0);
  }

  async onUseQuickMessage(quickMessage: QuickMessage) {
    await this.apiService.sendUserChatMessage(quickMessage.message);
    await this.apiService.generateAIChatMessage();
    this.router.navigate(['/chat-log']);
    // TODO: automatically navigate to chat log
  }
}
