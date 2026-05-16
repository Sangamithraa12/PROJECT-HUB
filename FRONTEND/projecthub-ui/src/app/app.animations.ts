import { trigger, transition, style, query, animate, group } from '@angular/animations';

export const routeTransitionAnimations = trigger('triggerName', [
  transition('* <=> *', [
    style({ position: 'relative' }),
    query(':enter, :leave', [
      style({
        position: 'absolute',
        top: 0,
        left: 0,
        width: '100%',
        opacity: 0
      })
    ], { optional: true }),
    query(':enter', [
      style({ opacity: 0, transform: 'translateY(10px)' })
    ], { optional: true }),
    group([
      query(':leave', [
        animate('0.3s ease-out', style({ opacity: 0, transform: 'translateY(-10px)' }))
      ], { optional: true }),
      query(':enter', [
        animate('0.4s 0.2s ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ], { optional: true })
    ])
  ])
]);
 
