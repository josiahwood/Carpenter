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
import { AppRoutingModule } from './app-routing.module';
import { MessageGenerationsComponent } from './message-generations/message-generations.component';

@NgModule({
  declarations: [
    AppComponent,
    ChatLogComponent,
    SignInComponent,
    ChatMemoryComponent,
    NavigationComponent,
    MessageGenerationsComponent
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
    MatProgressSpinnerModule,
    AppRoutingModule
  ],
  providers: [{ provide: BASE_PATH, useValue: 'https://zealous-wave-0e26a4710.3.azurestaticapps.net/api' }],
  bootstrap: [AppComponent]
})
export class AppModule { }
