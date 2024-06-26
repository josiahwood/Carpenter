import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { ApiModule } from './carpenter-api-client/api.module'
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { ChatLogComponent } from './chat-log/chat-log.component';
import { SignInComponent } from './sign-in/sign-in.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ChatMemoryComponent } from './chat-memory/chat-memory.component';
import { BASE_PATH } from './carpenter-api-client';
import { NavigationComponent } from './navigation/navigation.component';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'
import { MatExpansionModule } from '@angular/material/expansion'
import { MatDialogModule } from '@angular/material/dialog'
import { AppRoutingModule } from './app-routing.module';
import { MessageGenerationsComponent } from './message-generations/message-generations.component';
import { ChatMessageDialogComponent } from './chat-message-dialog/chat-message-dialog.component';
import { ImportChatLogComponent } from './import-chat-log/import-chat-log.component';
import { QuickMessagesComponent } from './quick-messages/quick-messages.component';
import { QuickMessageDialogComponent } from './quick-message-dialog/quick-message-dialog.component';
import { ChatSummarizationPromptComponent } from './chat-summarization-prompt/chat-summarization-prompt.component';
import { LandingComponent } from './landing/landing.component';
import { ChatInstructionComponent } from './chat-instruction/chat-instruction.component';
import { ChatSummaryLogsComponent } from './chat-summary-logs/chat-summary-logs.component';
import { ChatContextComponent } from './chat-context/chat-context.component';
import { ChatContextsComponent } from './chat-contexts/chat-contexts.component';
import { DeleteUserDataComponent } from './delete-user-data/delete-user-data.component';
import { ChatAuthorsNoteComponent } from './chat-authors-note/chat-authors-note.component';

@NgModule({
  declarations: [
    AppComponent,
    ChatLogComponent,
    SignInComponent,
    ChatMemoryComponent,
    NavigationComponent,
    MessageGenerationsComponent,
    ChatMessageDialogComponent,
    ImportChatLogComponent,
    QuickMessagesComponent,
    QuickMessageDialogComponent,
    ChatSummarizationPromptComponent,
    LandingComponent,
    ChatInstructionComponent,
    ChatSummaryLogsComponent,
    ChatContextComponent,
    ChatContextsComponent,
    DeleteUserDataComponent,
    ChatAuthorsNoteComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    ApiModule,
    // make sure to import the HttpClientModule in the AppModule only,
    // see https://github.com/angular/angular/issues/20575
    HttpClientModule,
    MatToolbarModule,
    MatButtonModule,
    MatSidenavModule,
    MatIconModule,
    MatListModule,
    MatCardModule,
    MatExpansionModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    AppRoutingModule
  ],
  providers: [{ provide: BASE_PATH, useValue: 'https://zealous-wave-0e26a4710.3.azurestaticapps.net/api' }],
  bootstrap: [AppComponent]
})
export class AppModule { }
