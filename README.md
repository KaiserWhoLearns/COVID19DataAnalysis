# COVID19DataAnalysis

## Setup

Download the repository using git with the submodules by doing:

```
git clone --recursive git@github.com:KaiserWhoLearns/COVID19DataAnalysis.git
```

If you already have the main branch of the repository, Do the following to fetch the JHU data:

```
git submodule init
git submodule update
```

To keep data synchronized perform:

```
git pull origin main
git submodule sync
```

## Necessary Python Tooling

We recommend using Anaconda (`conda`) as the easiest way to setup the Python environment necessary
for reproducing the various results. Install it by downloading the individual version from [here](https://www.anaconda.com/products/individual).

Once setup, ensure that `python` and `pip` are available on `PATH`.


## Script Documentation

| Script Name | Description |
|-------------|-------------|
| `prepare_covariants_data.py` | Uses the `data/covariants/` data to generate intermediate covariant data per country, area charts. |
| `visualize_case_death_heatmap.py` |             |
|             |             |
