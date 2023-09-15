import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MessageGenerationsComponent } from './message-generations.component';

describe('MessageGenerationsComponent', () => {
  let component: MessageGenerationsComponent;
  let fixture: ComponentFixture<MessageGenerationsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MessageGenerationsComponent]
    });
    fixture = TestBed.createComponent(MessageGenerationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
