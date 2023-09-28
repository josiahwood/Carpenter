import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatInstructionComponent } from './chat-instruction.component';

describe('ChatInstructionComponent', () => {
  let component: ChatInstructionComponent;
  let fixture: ComponentFixture<ChatInstructionComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChatInstructionComponent]
    });
    fixture = TestBed.createComponent(ChatInstructionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
