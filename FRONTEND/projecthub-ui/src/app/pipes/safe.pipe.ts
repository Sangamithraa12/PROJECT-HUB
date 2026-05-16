import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml, SafeResourceUrl, SafeScript, SafeStyle, SafeUrl } from '@angular/platform-browser';

@Pipe({
  name: 'safe',
  standalone: true
})
export class SafePipe implements PipeTransform {

  constructor(protected sanitizer: DomSanitizer) {}

  public transform(value: any, type: string): SafeHtml | SafeStyle | SafeScript | SafeUrl | SafeResourceUrl {
    switch (type) {
      case 'html': return this.sanitizer.bypassSecurityTrustHtml(value);
      case 'style': return this.sanitizer.bypassSecurityTrustStyle(value);
      case 'script': return this.sanitizer.bypassSecurityTrustScript(value);
      case 'url': return this.sanitizer.bypassSecurityTrustUrl(value);
      case 'resourceHtml': return this.sanitizer.bypassSecurityTrustResourceUrl(value);
      default: throw new Error(`Invalid safe type specified: ${type}`);
    }
  }
}

@Pipe({
  name: 'safeHtml',
  standalone: true
})
export class SafeHtmlPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}
  transform(html: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }
}

@Pipe({
  name: 'safeUrl',
  standalone: true
})
export class SafeUrlPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}
  transform(url: string | undefined): SafeResourceUrl {
    if (!url) return '';
    let finalUrl = url;
    if (url.includes('youtube.com/watch?v=')) {
      finalUrl = url.replace('watch?v=', 'embed/');
    } else if (url.includes('youtu.be/')) {
      finalUrl = url.replace('youtu.be/', 'youtube.com/embed/');
    }
    return this.sanitizer.bypassSecurityTrustResourceUrl(finalUrl);
  }
}
 
