import { Component } from '@angular/core';
import { CarpenterApiService } from '../carpenter-api.service';

@Component({
  selector: 'app-import-chat-log',
  templateUrl: './import-chat-log.component.html',
  styleUrls: ['./import-chat-log.component.css']
})
export class ImportChatLogComponent {
  public chatLog: string = "";

  constructor(private apiService: CarpenterApiService) {
  }

  public onChatLogValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.chatLog = value;
  }

  onImportChatLog(): void {
    this.apiService.importChatLog(this.chatLog);
  }
}
