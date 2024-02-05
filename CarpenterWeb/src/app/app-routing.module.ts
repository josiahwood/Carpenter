import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ChatLogComponent } from './chat-log/chat-log.component';
import { ChatMemoryComponent } from './chat-memory/chat-memory.component';
import { ChatAuthorsNoteComponent } from './chat-authors-note/chat-authors-note.component';
import { MessageGenerationsComponent } from './message-generations/message-generations.component';
import { ImportChatLogComponent } from './import-chat-log/import-chat-log.component';
import { QuickMessagesComponent } from './quick-messages/quick-messages.component';
import { ChatSummarizationPromptComponent } from './chat-summarization-prompt/chat-summarization-prompt.component';
import { LandingComponent } from './landing/landing.component';
import { ChatInstructionComponent } from './chat-instruction/chat-instruction.component';
import { ChatSummaryLogsComponent } from './chat-summary-logs/chat-summary-logs.component';
import { ChatContextsComponent } from './chat-contexts/chat-contexts.component';
import { DeleteUserDataComponent } from './delete-user-data/delete-user-data.component';

const routes: Routes = [
  { path: '', component: LandingComponent, pathMatch: "full" },
  { path: 'chat-log', component: ChatLogComponent },
  { path: 'quick-messages', component: QuickMessagesComponent },
  { path: 'chat-contexts', component: ChatContextsComponent },
  { path: 'chat-instruction', component: ChatInstructionComponent },
  { path: 'chat-memory', component: ChatMemoryComponent },
  { path: 'chat-authors-note', component: ChatAuthorsNoteComponent },
  { path: 'chat-summary-logs', component: ChatSummaryLogsComponent },
  { path: 'chat-summarization-prompt', component: ChatSummarizationPromptComponent },
  { path: 'message-generations', component: MessageGenerationsComponent },
  { path: 'import-chat-log', component: ImportChatLogComponent },
  { path: 'delete-user-data', component: DeleteUserDataComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
