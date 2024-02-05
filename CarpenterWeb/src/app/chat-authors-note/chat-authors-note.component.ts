import { Component } from '@angular/core';
import { CarpenterApiService } from '../carpenter-api.service';

@Component({
  selector: 'app-chat-authors-note',
  templateUrl: './chat-authors-note.component.html',
  styleUrls: ['./chat-authors-note.component.css']
})
export class ChatAuthorsNoteComponent {
  public chatAuthorsNote: string = "";

  constructor(private apiService: CarpenterApiService) {
  }

  async ngOnInit() {
    this.chatAuthorsNote = await this.apiService.getChatAuthorsNote();
  }

  public onChatAuthorsNoteValueChange(event: Event): void {
    console.log(event.target);
    const value = (event.target as any).value;
    this.chatAuthorsNote = value;
  }

  onSetAuthorsNote(): void {
    this.apiService.setChatAuthorsNote(this.chatAuthorsNote);
  }
}
