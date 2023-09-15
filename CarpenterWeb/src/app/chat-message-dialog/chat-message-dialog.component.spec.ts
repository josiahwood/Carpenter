import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatMessageDialogComponent } from './chat-message-dialog.component';

describe('ChatMessageDialogComponent', () => {
  let component: ChatMessageDialogComponent;
  let fixture: ComponentFixture<ChatMessageDialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChatMessageDialogComponent]
    });
    fixture = TestBed.createComponent(ChatMessageDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
