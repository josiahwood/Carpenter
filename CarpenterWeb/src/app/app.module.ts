import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { ChatLogComponent } from './chat-log/chat-log.component';
import { SignInComponent } from './sign-in/sign-in.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ChatMemoryComponent } from './chat-memory/chat-memory.component';

@NgModule({
  declarations: [
    AppComponent,
    ChatLogComponent,
    SignInComponent,
    ChatMemoryComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
