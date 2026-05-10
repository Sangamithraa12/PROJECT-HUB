import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from, map, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private apiKey: string = 'AIzaSyCktv2jlgRqapQFGWYwrCVgza03oBKU1ow'; 
  private baseUrl: string = 'https://generativelanguage.googleapis.com/v1beta';
  private activeModel: string = 'gemini-1.5-flash'; // Default guess

  constructor(private http: HttpClient) {}

  getAiResponse(query: string): Observable<string> {
    if (!this.apiKey || this.apiKey === 'YOUR_GEMINI_API_KEY_HERE') {
      return of("Please set your Gemini API Key to get real answers!");
    }

    // Wrap the whole thing in a promise to allow 'await' for the model check
    const promise = this.executeWithModelDiscovery(query);

    return from(promise).pipe(
      catchError(error => {
        console.error('Final AI Error:', error);
        return of(`AI Error: ${error.message}. Please check if the "Generative Language API" is enabled in your Google Cloud Console.`);
      })
    );
  }

  private async executeWithModelDiscovery(query: string): Promise<string> {
    try {
      // Step 1: Try with current active model
      return await this.callGemini(this.activeModel, query);
    } catch (err: any) {
      // Step 2: If model not found, try to discover what's available
      if (err.message.includes('not found') || err.message.includes('404')) {
        console.log('Model not found, discovering available models...');
        try {
          const modelsRes = await fetch(`${this.baseUrl}/models?key=${this.apiKey}`);
          const modelsData = await modelsRes.json();
          
          if (modelsData.models && modelsData.models.length > 0) {
            // Pick the first model that supports generateContent
            const bestModel = modelsData.models.find((m: any) => 
              m.supportedGenerationMethods.includes('generateContent') && 
              (m.name.includes('flash') || m.name.includes('pro'))
            );
            
            if (bestModel) {
              const modelName = bestModel.name.split('/').pop();
              console.log('Discovered best model:', modelName);
              this.activeModel = modelName;
              return await this.callGemini(this.activeModel, query);
            }
          }
        } catch (discoveryErr) {
          console.error('Model discovery failed:', discoveryErr);
        }
      }
      throw err;
    }
  }

  private async callGemini(model: string, query: string): Promise<string> {
    const payload = {
      contents: [{ parts: [{ text: query }] }]
    };

    const response = await fetch(`${this.baseUrl}/models/${model}:generateContent?key=${this.apiKey}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    const data = await response.json();
    if (!response.ok) {
      throw new Error(data.error?.message || 'Request failed');
    }

    if (data.candidates && data.candidates[0].content.parts[0].text) {
      return data.candidates[0].content.parts[0].text;
    }
    throw new Error('Empty response from AI');
  }
}
