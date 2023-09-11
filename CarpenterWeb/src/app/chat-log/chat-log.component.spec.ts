import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatLogComponent } from './chat-log.component';

describe('ChatLogComponent', () => {
  let component: ChatLogComponent;
  let fixture: ComponentFixture<ChatLogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChatLogComponent]
    });
    fixture = TestBed.createComponent(ChatLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
