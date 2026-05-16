import { Directive, ElementRef, HostListener, Renderer2 } from '@angular/core';

@Directive({
  selector: '[appRipple]',
  standalone: true
})
export class RippleDirective {
  constructor(private el: ElementRef, private renderer: Renderer2) {
    this.renderer.addClass(this.el.nativeElement, 'ripple-container');
  }

  @HostListener('mousedown', ['$event'])
  onMouseDown(event: MouseEvent) {
    const button = this.el.nativeElement;
    const ripple = this.renderer.createElement('span');
    
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;

    this.renderer.setStyle(ripple, 'width', `${size}px`);
    this.renderer.setStyle(ripple, 'height', `${size}px`);
    this.renderer.setStyle(ripple, 'left', `${x}px`);
    this.renderer.setStyle(ripple, 'top', `${y}px`);
    this.renderer.addClass(ripple, 'ripple');

    this.renderer.appendChild(button, ripple);

    setTimeout(() => {
      this.renderer.removeChild(button, ripple);
    }, 600);
  }
}
