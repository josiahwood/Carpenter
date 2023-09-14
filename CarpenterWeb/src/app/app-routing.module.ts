import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ChatLogComponent } from './chat-log/chat-log.component';
import { ChatMemoryComponent } from './chat-memory/chat-memory.component';

const routes: Routes = [
  { path: 'chat-log', component: ChatLogComponent },
  { path: 'chat-memory', component: ChatMemoryComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
