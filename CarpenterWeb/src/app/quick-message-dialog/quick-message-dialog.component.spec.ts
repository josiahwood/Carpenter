import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuickMessageDialogComponent } from './quick-message-dialog.component';

describe('QuickMessageDialogComponent', () => {
  let component: QuickMessageDialogComponent;
  let fixture: ComponentFixture<QuickMessageDialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [QuickMessageDialogComponent]
    });
    fixture = TestBed.createComponent(QuickMessageDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
