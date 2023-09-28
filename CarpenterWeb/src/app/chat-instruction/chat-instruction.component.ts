import { Component } from '@angular/core';
import { CarpenterApiService } from '../carpenter-api.service';
import { MessageGeneration } from '../models/message-generation';

@Component({
  selector: 'app-chat-instruction',
  templateUrl: './chat-instruction.component.html',
  styleUrls: ['./chat-instruction.component.css']
})
export class ChatInstructionComponent {
  public chatInstruction: string = "";
  public instructionResponse: string = "";
  
  public isWaiting: boolean = false;

  constructor(private apiService: CarpenterApiService) {
  }

  async ngOnInit() {
    await this.onUpdateMessageGenerationStatus();
  }

  public onChatInstructionValueChange(event: Event): void {
    const value = (event.target as any).value;
    this.chatInstruction = value;
  }

  async onExecuteChatInstruction() {
    this.isWaiting = true;

    if (this.chatInstruction != "") {
      await this.apiService.executeChatInstruction(this.chatInstruction);
      //this.chatInstruction = "";
    }

    await this.onUpdateMessageGenerationStatus();
  }

  async onUpdateMessageGenerationStatus() {
    var messageGenerations: MessageGeneration[] = await this.apiService.getNotDoneMessageGenerations();

    if (messageGenerations.length == 0) {
      this.instructionResponse = await this.apiService.getLatestChatInstructionResponse();
      this.isWaiting = false;
    }
    else {
      this.isWaiting = true;
      setTimeout(async () => { await this.onUpdateMessageGenerationStatus(); }, 1000);
    }
  }
}
