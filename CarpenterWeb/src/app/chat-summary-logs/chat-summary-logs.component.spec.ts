import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatSummaryLogsComponent } from './chat-summary-logs.component';

describe('ChatSummaryLogsComponent', () => {
  let component: ChatSummaryLogsComponent;
  let fixture: ComponentFixture<ChatSummaryLogsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChatSummaryLogsComponent]
    });
    fixture = TestBed.createComponent(ChatSummaryLogsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
