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

@NgModule({
  declarations: [
    AppComponent,
    ChatLogComponent,
    SignInComponent,
    ChatMemoryComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    ApiModule,
    // make sure to import the HttpClientModule in the AppModule only,
    // see https://github.com/angular/angular/issues/20575
    HttpClientModule
  ],
  providers: [{ provide: BASE_PATH, useValue: 'https://zealous-wave-0e26a4710.3.azurestaticapps.net/api' }],
  bootstrap: [AppComponent]
})
export class AppModule { }
