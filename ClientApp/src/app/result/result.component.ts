import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-result',
  templateUrl: './result.component.html',
  styleUrls: ['./result.component.css']
})
export class ResultComponent {
  @Input() cityName: string = "";
  @Input() conditionsDescription: string = "";
  @Input() currentTemperatureFarenheit: number = 0;
  @Input() currentTemperatureFeelsLikeFarenheit: number = 0;
  @Input() windSpeedMph: number = 0;
  @Input() succeeded: boolean = false;
  @Input() apiErrorMessage: string = "The API was never called (sample data)";

  constructor() { }

}
