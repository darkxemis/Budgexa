import {
  Component,
  input,
  output,
  computed,
  inject,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SelectorOptionInt } from '../../../core/models/selector.model';
import { UnitMeasure } from '../../models/unit-measure.model';
import { toSignal } from '@angular/core/rxjs-interop';
import { combineLatest } from 'rxjs';

export { UnitMeasure };

@Component({
  selector: 'app-unit-measure-selector',
  standalone: true,
  imports: [FormsModule, TranslateModule],
  templateUrl: './unit-measure-selector.component.html',
  styleUrl: './unit-measure-selector.component.scss'
})
export class UnitMeasureSelectorComponent {
  private readonly translate = inject(TranslateService);

  value = input<UnitMeasure | null>(null);
  disabled = input(false);
  placeholder = input('shared.unitMeasure.select');

  valueChange = output<UnitMeasure | null>();

  private readonly unitMeasureNames = toSignal(
    combineLatest([
      this.translate.stream('shared.unitMeasure.quantity'),
      this.translate.stream('shared.unitMeasure.time'),
      this.translate.stream('shared.unitMeasure.weight'),
    ]),
    { initialValue: ['Quantity', 'Time', 'Weight'] as [string, string, string] }
  );

  protected readonly unitMeasureOptions = computed(() => {
    const [quantity, time, weight] = this.unitMeasureNames();
    return [
      { id: UnitMeasure.Quantity, name: quantity },
      { id: UnitMeasure.Time, name: time },
      { id: UnitMeasure.Weight, name: weight },
    ];
  });

  protected onValueChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const value = target.value;

    if (value === '' || value === null || value === undefined) {
      this.valueChange.emit(null);
      return;
    }

    const parsedValue = parseInt(value, 10);
    if (isNaN(parsedValue)) return;
    this.valueChange.emit(parsedValue as UnitMeasure);
  }
}
