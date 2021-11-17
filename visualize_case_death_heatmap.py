import matplotlib.pyplot as plt
import pandas
import seaborn as sns
from scipy.stats import pearsonr

from data import loader
from helpers import heatmap


def plot_multi_covid_case_heatmap_data(multi_heatmap_result, days_counted):
    cases = multi_heatmap_result['cases']
    deaths = multi_heatmap_result['deaths']

    continent_wise_countries = loader.get_continent_wise_countries()

    for continent, country_names in continent_wise_countries.items():
        print(f"Generating heatmap for {continent}")
        fig, ax = plt.subplots(nrows=1, ncols=len(multi_heatmap_result), figsize=(20, 15), sharey=True)
        day_sequence = list(range(days_counted))
        case_data_ordered = []
        death_data_ordered = []
        for country in country_names:
            c = cases[country]
            d = deaths[country]
            case_data_ordered.append(c)
            death_data_ordered.append(d)

        case_heat_df = pandas.DataFrame(data=case_data_ordered, index=country_names, columns=day_sequence)
        death_heat_df = pandas.DataFrame(data=death_data_ordered, index=country_names, columns=day_sequence)
        sns.heatmap(case_heat_df, annot=False, ax=ax[0], rasterized=True, cbar_kws={"orientation": "horizontal"})
        sns.heatmap(death_heat_df, annot=False, ax=ax[1], rasterized=True, cbar_kws={"orientation": "horizontal"})
        ax[0].set_title('Cases per {}'.format(heatmap.get_normalized_per_population_value()))
        ax[1].set_title('Deaths per {}'.format(heatmap.get_normalized_per_population_value()))
        plt.tight_layout()
        plt.savefig(f'results/{continent}_heatmap.png', bbox_inches='tight')
        plt.close('all')


def plot_case_and_death_correlation_per_country(multi_heatmap_result):
    cases = multi_heatmap_result['cases']
    deaths = multi_heatmap_result['deaths']

    continent_wise_countries = loader.get_continent_wise_countries()

    for continent, country_names in continent_wise_countries.items():
        print(f"Generating case and death correlations for {continent}")
        fig, ax = plt.subplots(nrows=1, ncols=len(multi_heatmap_result), figsize=(20, 20), sharey=True)
        X_labels = country_names
        Y_labels = country_names

        case_matrix = []
        death_matrix = []
        for x_country in X_labels:
            x_case_values = cases[x_country]
            x_death_values = deaths[x_country]
            case_row = []
            death_row = []
            for y_country in Y_labels:
                y_case_values = cases[y_country]
                y_death_values = deaths[y_country]
                case_r, _case_p = pearsonr(x_case_values, y_case_values)
                death_r, _death_p = pearsonr(x_death_values, y_death_values)
                case_row.append(case_r)
                death_row.append(death_r)
            case_matrix.append(case_row)
            death_matrix.append(death_row)

        case_corr_df = pandas.DataFrame(data=case_matrix, index=X_labels, columns=Y_labels)
        death_corr_df = pandas.DataFrame(data=death_matrix, index=X_labels, columns=Y_labels)

        sns.heatmap(case_corr_df, annot=False, ax=ax[0], rasterized=True, cbar_kws={"orientation": "horizontal"})
        sns.heatmap(death_corr_df, annot=False, ax=ax[1], rasterized=True, cbar_kws={"orientation": "horizontal"})

        ax[0].set_title('Correlation of Cases per {}'.format(heatmap.get_normalized_per_population_value()))
        ax[1].set_title('Correlation of Deaths per {}'.format(heatmap.get_normalized_per_population_value()))

        plt.savefig(f'results/{continent}_correlation_heatmap.png', bbox_inches='tight')
        plt.close('all')


if __name__ == '__main__':
    cases, deaths = loader.get_global_case_and_deaths_time_series_data()
    time_series_columns = cases.columns[len(heatmap.get_non_time_series_columns()):]
    cases_heatmap_data = heatmap.process_heatmap_data(cases)
    deaths_heatmap_data = heatmap.process_heatmap_data(deaths)
    multi_heatmap_result = {
        'cases': cases_heatmap_data,
        'deaths': deaths_heatmap_data,
    }
    plot_multi_covid_case_heatmap_data(multi_heatmap_result, len(time_series_columns))
    plot_case_and_death_correlation_per_country(multi_heatmap_result)
