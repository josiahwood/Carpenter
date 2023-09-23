import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuickMessagesComponent } from './quick-messages.component';

describe('QuickMessagesComponent', () => {
  let component: QuickMessagesComponent;
  let fixture: ComponentFixture<QuickMessagesComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [QuickMessagesComponent]
    });
    fixture = TestBed.createComponent(QuickMessagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
