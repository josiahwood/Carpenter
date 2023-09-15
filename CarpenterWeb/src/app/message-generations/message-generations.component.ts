import { Component, ElementRef, ViewChild } from '@angular/core';
import { MessageGeneration } from '../models/message-generation';
import { CarpenterApiService } from '../carpenter-api.service';

@Component({
  selector: 'app-message-generations',
  templateUrl: './message-generations.component.html',
  styleUrls: ['./message-generations.component.css']
})
export class MessageGenerationsComponent {
  @ViewChild('messageGenerationsDiv') messageGenerationsDiv!: ElementRef;
  public messageGenerations: MessageGeneration[] = [];

  constructor(private apiService: CarpenterApiService) {
  }

  async ngOnInit() {
    this.messageGenerations = await this.apiService.getMessageGenerations();
    setTimeout(() => {
      this.messageGenerationsDiv.nativeElement.scrollTop = this.messageGenerationsDiv.nativeElement.scrollHeight;
    }, 0);
  }
}
