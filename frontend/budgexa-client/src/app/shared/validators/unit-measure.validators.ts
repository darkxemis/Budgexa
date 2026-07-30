import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { UnitMeasure } from '../models/unit-measure.model';

const STEP_TIME = 0.25;
const STEP_QUANTITY = 0.50;
const MIN_QUANTITY = 0.50;

export function quantityByUnitMeasureValidator(unitMeasureControl: AbstractControl<UnitMeasure | null>): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const unitMeasure = unitMeasureControl.value;
    const quantity = control.value as number | undefined;

    if (quantity === undefined || quantity === null || quantity === 0) {
      return null;
    }

    if (quantity < 0) {
      return { negative: true };
    }

    if (quantity === 0) {
      return null;
    }

    switch (unitMeasure) {
      case UnitMeasure.Time: {
        const scaled = Math.round(quantity * 100);
        const stepScaled = Math.round(STEP_TIME * 100);
        if (scaled % stepScaled !== 0) {
          return { invalidStep: { expected: STEP_TIME } };
        }
        break;
      }
      case UnitMeasure.Quantity: {
        if (quantity < MIN_QUANTITY) {
          return { minQuantity: { min: MIN_QUANTITY } };
        }
        const scaled = Math.round(quantity * 100);
        const stepScaled = Math.round(STEP_QUANTITY * 100);
        if (scaled % stepScaled !== 0) {
          return { invalidStep: { expected: STEP_QUANTITY } };
        }
        break;
      }
      case UnitMeasure.Weight:
      case null:
      default:
        break;
    }

    return null;
  };
}

export function lineQuantityUnitMeasureValidator(group: AbstractControl): ValidationErrors | null {
  const g = group as AbstractControl<{ quantity: number; unitMeasure: UnitMeasure | null }>;
  const unitMeasure = g.get('unitMeasure')?.value as UnitMeasure | null;
  const quantity = g.get('quantity')?.value as number | undefined;

  if (quantity === undefined || quantity === null || quantity === 0) {
    return null;
  }

  if (quantity < 0) {
    return { quantityNegative: true };
  }

  if (quantity === 0) {
    return null;
  }

  switch (unitMeasure) {
    case UnitMeasure.Time: {
      const scaled = Math.round(quantity * 100);
      const stepScaled = Math.round(STEP_TIME * 100);
      if (scaled % stepScaled !== 0) {
        return { quantityInvalidStep: { expected: STEP_TIME } };
      }
      break;
    }
    case UnitMeasure.Quantity: {
      if (quantity < MIN_QUANTITY) {
        return { quantityMin: { min: MIN_QUANTITY } };
      }
      const scaled = Math.round(quantity * 100);
      const stepScaled = Math.round(STEP_QUANTITY * 100);
      if (scaled % stepScaled !== 0) {
        return { quantityInvalidStep: { expected: STEP_QUANTITY } };
      }
      break;
    }
    case UnitMeasure.Weight:
    case null:
    default:
      break;
  }

  return null;
}
