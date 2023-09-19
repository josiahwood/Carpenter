import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ChatLogComponent } from './chat-log/chat-log.component';
import { ChatMemoryComponent } from './chat-memory/chat-memory.component';
import { MessageGenerationsComponent } from './message-generations/message-generations.component';
import { ImportChatLogComponent } from './import-chat-log/import-chat-log.component';

const routes: Routes = [
  { path: 'chat-log', component: ChatLogComponent },
  { path: 'chat-memory', component: ChatMemoryComponent },
  { path: 'message-generations', component: MessageGenerationsComponent },
  { path:'import-chat-log', component: ImportChatLogComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
