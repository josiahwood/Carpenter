import { Component } from '@angular/core';
import { NameService } from '../carpenter-api-client';

@Component({
  selector: 'app-chat-memory',
  templateUrl: './chat-memory.component.html',
  styleUrls: ['./chat-memory.component.css']
})
export class ChatMemoryComponent {
  public chatMemory = "";

  constructor(private nameService: NameService) {
  }

  onSetMemory(): void {
    console.log("onSetMemory");
    console.log(this.chatMemory);

    this.nameService.run_1(this.chatMemory);
  }
}
