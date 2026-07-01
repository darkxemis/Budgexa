import {
  Component,
  signal,
  computed,
  viewChild,
  ElementRef,
  AfterViewInit,
  effect,
  input,
  output,
  ChangeDetectionStrategy,
} from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { IconComponent } from '../icon/icon.component';
import { SelectorOption } from '../../../core/models/selector.model';
import { Guid } from '../../../core/models/guid.model';

@Component({
  selector: 'app-autocomplete-selector',
  standalone: true,
  imports: [TranslateModule, IconComponent],
  templateUrl: './autocomplete-selector.component.html',
  styleUrl: './autocomplete-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AutocompleteSelectorComponent implements AfterViewInit {
  options = input<SelectorOption[] | ((searchQuery?: string) => Promise<SelectorOption[]>)>([]);
  placeholder = input('grid.searchOrSelect');
  initialValue = input<Guid | null>(null);
  disabled = input(false);

  valueChange = output<Guid | null>();

  searchInput = viewChild.required<ElementRef<HTMLInputElement>>('searchInput');

  protected readonly searchText = signal('');
  protected readonly availableOptions = signal<SelectorOption[]>([]);
  protected readonly loading = signal(false);
  protected readonly showDropdown = signal(false);
  protected readonly selectedOption = signal<SelectorOption | null>(null);
  protected readonly highlightedIndex = signal(-1);

  private _blurTimer?: ReturnType<typeof setTimeout>;
  private _debounceTimer?: ReturnType<typeof setTimeout>;
  private readonly debounceMs = 300;
  // Tracks whether initialValue has been hydrated to avoid re-clearing on every render
  private _initialised = false;

  protected readonly filteredOptions = computed(() => {
    const options = this.availableOptions();
    const search = this.searchText().toLowerCase();
    // When an option is selected, show the full list (not filtered by the selected name)
    if (!search || this.selectedOption()) return options;
    return options.filter(opt => opt.name.toLowerCase().includes(search));
  });

  constructor() {
    // Hydrate the display text from initialValue only once per distinct non-null value.
    // We must NOT clear searchText when initialValue goes null — that would erase
    // text the user is actively typing.
    effect(async () => {
      const initVal = this.initialValue();
      if (initVal && !this._initialised) {
        this._initialised = true;
        await this.setDisplayValueFromId(initVal);
      } else if (!initVal && this._initialised) {
        // Value was explicitly cleared externally (e.g. parent reset)
        this._initialised = false;
        this.searchText.set('');
        this.selectedOption.set(null);
      }
    });
  }

  async ngAfterViewInit(): Promise<void> {
    await this.loadOptions();
  }

  // ----------------------------------------------------------------
  // Input events
  // ----------------------------------------------------------------

  protected onInput(value: string): void {
    this.searchText.set(value);
    this.highlightedIndex.set(-1);

    // If user types over a selection, discard the internal selection
    // but do NOT emit valueChange — we only emit on explicit actions
    // (selectOption or clearSelection) to avoid parent state churn.
    if (this.selectedOption()) {
      this.selectedOption.set(null);
    }

    if (!this.showDropdown()) {
      this.showDropdown.set(true);
    }

    clearTimeout(this._debounceTimer);
    this._debounceTimer = setTimeout(async () => {
      await this.loadOptions(value || undefined);
    }, this.debounceMs);
  }

  protected onFocus(): void {
    this.showDropdown.set(true);
  }

  // onBlur fires when the input loses focus.
  // We delay closing so that a mousedown on an option can cancel the timer
  // and register the click before the dropdown disappears.
  protected onBlur(): void {
    this._blurTimer = setTimeout(() => {
      this.showDropdown.set(false);
      this.highlightedIndex.set(-1);
    }, 150);
  }

  // Called via (mousedown) on the dropdown panel — prevents the blur from
  // closing the dropdown before the (click) on an option fires.
  protected cancelBlur(): void {
    clearTimeout(this._blurTimer);
  }

  protected onKeyDown(event: KeyboardEvent): void {
    const options = this.filteredOptions();
    const idx = this.highlightedIndex();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (!this.showDropdown()) {
          this.showDropdown.set(true);
        } else {
          this.highlightedIndex.set(idx < options.length - 1 ? idx + 1 : 0);
        }
        break;

      case 'ArrowUp':
        event.preventDefault();
        if (this.showDropdown()) {
          this.highlightedIndex.set(idx > 0 ? idx - 1 : options.length - 1);
        }
        break;

      case 'Enter':
        event.preventDefault();
        if (this.showDropdown() && idx >= 0 && options[idx]) {
          this.selectOption(options[idx]);
        }
        break;

      case 'Escape':
        event.preventDefault();
        this.showDropdown.set(false);
        this.highlightedIndex.set(-1);
        break;
    }
  }

  // ----------------------------------------------------------------
  // Selection actions
  // ----------------------------------------------------------------

  protected selectOption(option: SelectorOption): void {
    clearTimeout(this._blurTimer);
    this.selectedOption.set(option);
    this.searchText.set(option.name);
    this.showDropdown.set(false);
    this.highlightedIndex.set(-1);
    this.valueChange.emit(option.id);
  }

  protected clearSelection(): void {
    clearTimeout(this._blurTimer);
    this._initialised = false;
    this.searchText.set('');
    this.selectedOption.set(null);
    this.showDropdown.set(false);
    this.valueChange.emit(null);
    // Reload full list so user can pick again immediately
    void this.loadOptions();
  }

  // ----------------------------------------------------------------
  // Private helpers
  // ----------------------------------------------------------------

  private async loadOptions(searchQuery?: string): Promise<void> {
    this.loading.set(true);
    try {
      const opts = this.options();
      if (typeof opts === 'function') {
        this.availableOptions.set(await opts(searchQuery));
      } else {
        this.availableOptions.set(opts);
      }
    } catch {
      this.availableOptions.set([]);
    } finally {
      this.loading.set(false);
    }
  }

  private async setDisplayValueFromId(id: Guid): Promise<void> {
    if (this.availableOptions().length === 0) {
      await this.loadOptions();
    }
    const option = this.availableOptions().find(opt => opt.id === id);
    if (option) {
      this.selectedOption.set(option);
      this.searchText.set(option.name);
    }
  }
}
